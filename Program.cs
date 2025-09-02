using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

Console.OutputEncoding = Encoding.UTF8;

Console.WriteLine("=== Processador Assíncrono de Arquivos .TXT (.NET 8) ===");
Console.WriteLine("Informe o diretório que contém os arquivos .txt:");

string? dir;
while (true)
{
    Console.Write("> ");
    dir = Console.ReadLine();

    if (!string.IsNullOrWhiteSpace(dir) && Directory.Exists(dir))
        break;

    Console.WriteLine("Diretório inválido. Tente novamente.");
}

var txtFiles = Directory.EnumerateFiles(dir!, "*.txt", SearchOption.TopDirectoryOnly)
                        .OrderBy(p => p, StringComparer.InvariantCultureIgnoreCase)
                        .ToList();

if (txtFiles.Count == 0)
{
    Console.WriteLine("Nenhum arquivo .txt encontrado nesse diretório.");
    return;
}

Console.WriteLine($"\nArquivos encontrados ({txtFiles.Count}):");
for (int i = 0; i < txtFiles.Count; i++)
{
    Console.WriteLine($"{i + 1,3}. {Path.GetFileName(txtFiles[i])}");
}

Console.WriteLine("\nSelecione os arquivos para processar:");
Console.WriteLine("- Digite 'all' para processar todos");
Console.WriteLine("- Ou informe índices/intervalos (ex: 1,3-5,8)");
Console.Write("> ");
string? selection = Console.ReadLine()?.Trim();

HashSet<int> indexesToProcess = new();
if (string.Equals(selection, "all", StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(selection))
{
    for (int i = 1; i <= txtFiles.Count; i++) indexesToProcess.Add(i);
}
else
{
    foreach (var token in selection.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
    {
        if (token.Contains('-'))
        {
            var parts = token.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length == 2 && int.TryParse(parts[0], out int start) && int.TryParse(parts[1], out int end))
            {
                if (start > end) (start, end) = (end, start);
                for (int i = start; i <= end; i++)
                    if (i >= 1 && i <= txtFiles.Count) indexesToProcess.Add(i);
            }
        }
        else if (int.TryParse(token, out int idx))
        {
            if (idx >= 1 && idx <= txtFiles.Count) indexesToProcess.Add(idx);
        }
    }

    if (indexesToProcess.Count == 0)
    {
        Console.WriteLine("Nenhum índice válido informado. Encerrando.");
        return;
    }
}

var selectedFiles = indexesToProcess.OrderBy(i => i).Select(i => txtFiles[i - 1]).ToList();

Console.WriteLine($"\nIniciando processamento de {selectedFiles.Count} arquivo(s)...\n");

var wordRegex = new Regex(
    @"[\p{L}\p{N}]+",
    RegexOptions.Compiled);

// Resultados e tasks
var results = new ConcurrentBag<(string FileName, long Lines, long Words)>();
var tasks = new List<Task>();

foreach (var file in selectedFiles)
{
    tasks.Add(ProcessFileAsync(file, results));
}

await Task.WhenAll(tasks);

// Gera relatório
var exportDir = Path.Combine(AppContext.BaseDirectory, "export");
Directory.CreateDirectory(exportDir);
var reportPath = Path.Combine(exportDir, "relatorio.txt");

var linesOut = results
    .OrderBy(r => r.FileName, StringComparer.InvariantCultureIgnoreCase)
    .Select(r => $"{Path.GetFileName(r.FileName)} - {r.Lines} linhas - {r.Words} palavras");

await File.WriteAllLinesAsync(reportPath, linesOut, Encoding.UTF8);

Console.WriteLine($"\nConcluído! Relatório salvo em: {reportPath}");
Console.WriteLine("Pressione ENTER para sair.");
Console.ReadLine();

async Task ProcessFileAsync(string path, ConcurrentBag<(string FileName, long Lines, long Words)> sink)
{
    var fileName = Path.GetFileName(path);
    Console.WriteLine($"Processando: {fileName} ...");

    // --- Leitura com detecção de BOM e fallback para Latin1 se aparecer U+FFFD ---
    string text;
    await using (var fs1 = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 64 * 1024, FileOptions.Asynchronous))
    using (var r1 = new StreamReader(fs1, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), detectEncodingFromByteOrderMarks: true))
    {
        text = await r1.ReadToEndAsync();
    }
    if (text.IndexOf('\uFFFD') >= 0) // caractere de substituição => provável encoding ANSI/Latin-1
    {
        await using var fs2 = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 64 * 1024, FileOptions.Asynchronous);
        using var r2 = new StreamReader(fs2, Encoding.Latin1, detectEncodingFromByteOrderMarks: true);
        text = await r2.ReadToEndAsync();
    }

    // Remove BOM se presente no início
    if (text.Length > 0 && text[0] == '\uFEFF') text = text.Substring(1);

    // Normaliza quebras de linha para contar linhas com precisão
    // (não ignora a primeira linha, mesmo sem \n no fim do arquivo)
    long lineCount = 0;
    if (text.Length > 0)
    {
        foreach (var ch in text)
            if (ch == '\n') lineCount++;
        // Se o arquivo não termina com \n, ainda assim há a última linha
        if (text[^1] != '\n' && text[^1] != '\r') lineCount++;
        // Arquivos com apenas \r (raro) também contam 1 linha
        if (lineCount == 0) lineCount = 1;
    }

    // Contagem de palavras por transições "fora->dentro" de [letra ou dígito]
    long wordCount = 0;
    bool inToken = false;
    for (int i = 0; i < text.Length; i++)
    {
        char ch = text[i];

        if (!char.IsWhiteSpace(ch))
        {
            // Considera letras, dígitos e '=' como parte da palavra
            if (char.IsLetterOrDigit(ch) || ch == '=')
            {
                if (!inToken)
                {
                    inToken = true;
                    wordCount++;
                }
            }
            else
            {
                // Qualquer outro caractere encerra o token
                inToken = false;
            }
        }
        else
        {
            inToken = false;
        }
    }

    sink.Add((path, lineCount, wordCount));
    Console.WriteLine($"Concluído : {fileName} — {lineCount} linha(s), {wordCount} palavra(s).");
}
