# Processador Assíncrono de Arquivos .TXT (.NET 8)

Este projeto é uma aplicação **Console em C# (.NET 8)** desenvolvida
para a disciplina C-SHARP SOFTWARE DEVELOPMENT. O sistema permite selecionar arquivos
`.txt` de uma pasta e, de forma **assíncrona e paralela**, processar
cada um, contando o número de **linhas** e **palavras**.

------------------------------------------------------------------------

## ✨ Funcionalidades

-   Lista os arquivos `.txt` disponíveis em um diretório informado pelo
    usuário.
-   Permite selecionar todos os arquivos ou apenas alguns (por
    índice/intervalo).
-   Processa cada arquivo de forma **assíncrona**, exibindo mensagens de
    **"Processando"** e **"Concluído"** no console.
-   Conta corretamente **linhas** e **palavras**.
-   Gera um relatório consolidado (`relatorio.txt`) dentro da pasta
    **`/export`**.

------------------------------------------------------------------------

## 🚀 Como executar

1.  Clonar o repositório ou baixar os arquivos.

2.  Criar o projeto console:

    ``` bash
    dotnet new console -n ProcessaTxtAsync
    cd ProcessaTxtAsync
    ```

3.  Substituir o conteúdo de `Program.cs` pelo código fornecido.

4.  Rodar a aplicação:

    ``` bash
    dotnet run
    ```

------------------------------------------------------------------------

## 📂 Estrutura do projeto

    ProcessaTxtAsync/
     ├── Program.cs        # Código principal da aplicação
     ├── export/           # Pasta gerada automaticamente com o relatório
     └── README.md         # Este arquivo

------------------------------------------------------------------------

## 📝 Relatório

O relatório final é salvo em:

    ./export/relatorio.txt

Formato de cada linha:

    <nome-do-arquivo>.txt - X linhas - Y palavras

------------------------------------------------------------------------

## 👨‍💻 Autor

Projeto desenvolvido por: 
**Natan Eguchi dos Santos** - **RM98720**
**Kayky Paschoal Ribeiro** - **RM99929**
**João Pedro Marques Rodrigues** - **RM98307**

