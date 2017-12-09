
Para correr em modo teste:
	Correr o ProcessCreationService
	Correr o PuppetMaster

O PuppetMaster executa o ficheiro init que está na pasta scripts

Limitações:
	A solução implementa o RAFT para replicação. A nossa implementação tem alguns bugs e por vezes acontece no momento de consensus o servidor reenviar informação aos clientes, isto gera conflitos que não chegamos a corrigir.
	O algoritmo RAFT ficou um pouco lento e por vezes leva algum tempo a recuperar de falhas devido ao mecanismo de eleição.
