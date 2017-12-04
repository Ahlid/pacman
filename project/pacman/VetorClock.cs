using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;
using Shared;

namespace pacman
{
    public class VetorClock<T>
    {
        public int counter;
        public int Index { get; }
        public int[] vector;
        private Dictionary<IVectorMessage<T>, List<VectorDependencies>> waitingVectorMessages;
        private List<IVectorMessage<T>> Messages { get; set; }
        //receivedIndex + "" + receivedVersion
        private Dictionary<string, IVectorMessage<T>> hashVetorMessages;

        public VetorClock(int size, int index)
        {
            this.Index = index;
            this.vector = new int[size];
            this.Messages = new List<IVectorMessage<T>>();
            this.waitingVectorMessages = new Dictionary<IVectorMessage<T>, List<VectorDependencies>>();
            this.counter = 0;
            this.hashVetorMessages = new Dictionary<string, IVectorMessage<T>>();
            //fill others with zeros
            for (int i = 0; i < size; i++)
            {
                this.vector[i] = 0;
            }
        }

        public IVectorMessage<T> Tick(T message)
        {
            lock (this)
            {
                //incrementar counter
                this.vector[this.Index] = ++this.counter;
                VectorMessage<T> messageVectorMessage = new VectorMessage<T> { Index = this.Index, Vector = (int[])this.vector.Clone(), Message = message };

                this.Messages.Add(messageVectorMessage);
                this.hashVetorMessages[this.Index + "" + this.counter] = messageVectorMessage;

                return messageVectorMessage;
            }
        }

        public void ReceiveMessage(IVectorMessage<T> message)
        {
            lock (this)
            {
                int receivedIndex = message.Index; // o index de quem enviou a mensagem
                int receivedVersion =
                    message.Vector[receivedIndex]; //o numero do contador de quem enviou a mensagem nesta mensagem
                List<VectorDependencies>
                    dependencies = new List<VectorDependencies>(); //lista de dependencias para adiconar caso necessario




                //já recebeu esta mensagem
                if (this.hashVetorMessages.ContainsKey(receivedIndex + "" + receivedVersion))
                {
                    return;
                }

                //add to hash table
                this.hashVetorMessages[receivedIndex + "" + receivedVersion] = message;

                //vamos verificar se existe dependencias
                //percorremos todos os campos do vetor que este relogio tem(todos os relogios basicamente)
                for (int i = 0; i < this.vector.Length; i++)
                {
                    //se for o mesmo que este não vale apena comparar pois este tem sempre a versão mais recente de ele proprio
                    if (i == this.Index)
                    {
                        continue;
                    }

                    // se a versão for a do recebido
                    if (i == receivedIndex)
                    {
                        int actualReceivedVersion =
                            this.vector[
                                receivedIndex]; //vamos ver o valor do tick do relogio de quem nos enviou a mensagem

                        //existe versões não recebidas
                        //se a diferença for de 1 então esta tudo bem porque vamos receber a mensagem que nos vai atualizar para a versão mais recente 
                        //de quem enviou exemplo [1,2,0] comparado a [1,3,0] sendo quem enviou o index 1
                        if (receivedVersion - actualReceivedVersion != 1)
                        {
                            //adicionar as versões em falta as dependencias
                            //se não foi temos de adicionar todas as versões anteriores ás dependencias
                            // exemplo [1,2,0] comparado a [1,4,0] sendo quem enviou o index 1 tem dependencia da mensagem 3 do index 1
                            int aux = actualReceivedVersion;
                            do
                            {
                                aux++;
                                dependencies.Add(new VectorDependencies
                                {
                                    Index = receivedIndex,
                                    Version = aux
                                }); // adicionar as dependencias
                            } while (receivedVersion - aux != 1);
                        }

                        continue;
                    }

                    //existe mensagens que o que enviou tem e este não logo existe dependencia
                    //o caso as outras dependencias de outros relogios
                    //exemplo [1,2,0] comparado a [1,3,1] sendo quem enviou o index 1 tem dependencia do contador 1 no index 2
                    if (message.Vector[i] > this.vector[i])
                    {

                        //a versão atual
                        int aux = this.vector[i];
                        do
                        {
                            aux++;
                            dependencies.Add(new VectorDependencies { Index = i, Version = aux });
                        } while (message.Vector[i] != aux);

                    }
                }

                //verificar se existem dependencias

                if (dependencies.Count == 0)
                {
                    //não existem dependencias logo vamos adicionar a mensagem e dar update no relógio
                    this.vector[receivedIndex] = receivedVersion; // exemplo [1,2,0] => [1,3,0] 
                    this.Messages.Add(message);
                    //retirar dependencias desta mensagem
                    this.RemoveDependencie(receivedIndex, receivedVersion);
                }
                else
                {
                    this.waitingVectorMessages[message] = dependencies;
                }

            }
        }

        public void RemoveDependencie(int index, int version)
        {

            //por cada mensagem na lista de espera
            foreach (VectorMessage<T> message in this.waitingVectorMessages.Keys)
            {

                //vamos verificar se tem esta dependencia
                //aqui eu acabei de receber uma messagem no index = index e versão = versao
                //vou selecionar uma dependencia na lista que seja igual ao index e á versão
                VectorDependencies vetor = this.waitingVectorMessages[message]
                    .FirstOrDefault(m => m.Index == index && m.Version == version);//explica esta

                //ver se a messagem depende da que foi recebida
                if (vetor != null)
                {
                    this.waitingVectorMessages[message].Remove(vetor);

                    //ver se ficou sem dependencias
                    if (this.waitingVectorMessages[message].Count == 0)
                    {
                        this.vector[message.Index] = message.Vector[message.Index];
                        this.Messages.Add(message);
                        //retirar dependencias desta mensagem
                        this.RemoveDependencie(message.Index, message.Vector[message.Index]);
                    }
                }

            }

        }

        public List<IVectorMessage<T>> GetMissingMessages(int[] compareVetor)
        {
            lock (this)
            {
                List<IVectorMessage<T>> missingMessages = new List<IVectorMessage<T>>();

                for (int index = 0; index < this.vector.Length; index++)
                {
                    //se no index a versão é inferior
                    if (compareVetor[index] < this.vector[index])
                    {
                        int receivedVersion = compareVetor[index];
                        int expectedVersion = this.vector[index];

                        do
                        {
                            receivedVersion++;
                            IVectorMessage<T> message = this.hashVetorMessages[index + "" + receivedVersion];
                            missingMessages.Add(message);

                        } while (receivedVersion != expectedVersion);
                    }
                }

                return missingMessages;
            }
        }

        public List<T> GetMessages()
        {
            List<T> messagesList = new List<T>();

            foreach (VectorMessage<T> vectorMessage in this.Messages)
            {
                messagesList.Add(vectorMessage.Message);
            }

            return messagesList;

        }

    }
}