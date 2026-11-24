using System.CommandLine;
using DfE.GIAP.Verification;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    // The arguments are the commands', not configuration
    Args = [],
    ContentRootPath = AppContext.BaseDirectory
});

if (builder.Environment.IsEnvironment("Local"))
{
    builder.Configuration.AddUserSecrets(typeof(CompareCommand).Assembly, optional: true);
}

Dictionary<string, Check> checks = new()
{
    ["records"] = Check.Records,
    ["data-files"] = Check.DataFiles,
    ["downloads"] = Check.Downloads
};

Option<CompareScope> scopeOption = new("--scope")
{
    Description = "keys to compare only what's in the given zips; full to compare everything.",
    Required = true
};
Option<string[]> fileOption = new("--file")
{
    Description = "A zip to compare, as its ingest file path, e.g. ks2/NPD_KS2_2025_U.zip. Can be repeated.",
    Arity = ArgumentArity.ZeroOrMore
};
Option<string[]> legacyRunIdOption = new("--legacy-run-id")
{
    Description = "<data file name>=<run ID>: the old pipeline's run ID for a data file, to find its rejected rows. Can be repeated.",
    Arity = ArgumentArity.ZeroOrMore
};
Option<string[]> checkOption = new("--check")
{
    Description = "Only run these checks. Can be repeated; every check is run if there are none.",
    Arity = ArgumentArity.ZeroOrMore
};
checkOption.AcceptOnlyFromAmong([.. checks.Keys]);
Option<bool> detailOption = new("--detail")
{
    Description = "Also write every difference, with its values, to details.csv."
};
Option<int> batchSizeOption = new("--batch-size")
{
    Description = "How many records to read from each side at a time.",
    DefaultValueFactory = _ => 250
};
Option<int> concurrencyOption = new("--concurrency")
{
    Description = "How many batches to compare at once.",
    DefaultValueFactory = _ => 2
};
Option<double?> maxRequestUnitsOption = new("--max-ru")
{
    Description = "The most Cosmos DB request units to use a second. There's no limit if it isn't given."
};
Option<string?> outputOption = new("--output")
{
    Description = "A local folder to also write the report to."
};
Option<string?> rulesOption = new("--rules")
{
    Description = "The accepted differences file to use instead of the one shipped with the tool."
};

Command compareCommand = new("compare", "Compare what the new pipeline wrote with what the old pipeline wrote.")
{
    scopeOption,
    fileOption,
    legacyRunIdOption,
    checkOption,
    detailOption,
    batchSizeOption,
    concurrencyOption,
    maxRequestUnitsOption,
    outputOption,
    rulesOption
};

compareCommand.SetAction(async (parseResult, cancellationToken) =>
{
    try
    {
        string[] checkNames = parseResult.GetValue(checkOption) ?? [];

        CompareOptions options = new(
            parseResult.GetValue(scopeOption),
            parseResult.GetValue(fileOption) ?? [],
            ParseRunIds(parseResult.GetValue(legacyRunIdOption) ?? []),
            new HashSet<Check>(checkNames.Length == 0 ? checks.Values : checkNames.Select(c => checks[c])),
            parseResult.GetValue(detailOption),
            parseResult.GetValue(batchSizeOption),
            parseResult.GetValue(concurrencyOption),
            parseResult.GetValue(maxRequestUnitsOption),
            parseResult.GetValue(outputOption),
            parseResult.GetValue(rulesOption));

        return await CompareCommand.RunAsync(options, builder.Configuration, Console.Out, Console.Error, cancellationToken);
    }
    catch (Exception ex) when (ex is not OperationCanceledException)
    {
        await Console.Error.WriteLineAsync(ex.ToString());
        return CompareCommand.ErrorExitCode;
    }
});

Option<string?> keyVaultOption = new("--key-vault")
{
    Description = "The production app Key Vault to read the secrets from with your Azure CLI login, e.g. s189p01-giap-pd-app-kv. Without it, they're read from configuration, which only works with Azurite."
};

Option<string> manifestOption = new("--manifest")
{
    Description = "The manifest of zips to anonymise.",
    Required = true
};
Option<bool> listColumnsOption = new("--list-columns")
{
    Description = "Describe the zips' data files, without any of their values, rather than anonymise them."
};
Option<bool> overwriteOption = new("--overwrite")
{
    Description = "Replace anonymised zips that are already there."
};
Option<string?> classificationOption = new("--classification")
{
    Description = "The classification to use instead of the one shipped with the tool."
};

Command anonymiseCommand = new("anonymise", "Anonymise real zips from production into the test environment's verification container.")
{
    manifestOption,
    keyVaultOption,
    listColumnsOption,
    overwriteOption,
    classificationOption
};

anonymiseCommand.SetAction(async (parseResult, cancellationToken) =>
{
    try
    {
        AnonymiseOptions options = new(
            parseResult.GetValue(manifestOption)!,
            parseResult.GetValue(keyVaultOption),
            parseResult.GetValue(listColumnsOption),
            parseResult.GetValue(overwriteOption),
            parseResult.GetValue(classificationOption));

        return await AnonymiseCommand.RunAsync(options, builder.Configuration, Console.Out, Console.Error, cancellationToken);
    }
    catch (Exception ex) when (ex is not OperationCanceledException)
    {
        await Console.Error.WriteLineAsync(ex.ToString());
        return CompareCommand.ErrorExitCode;
    }
});

Option<string[]> folderOption = new("--folder")
{
    Description = "Only list this ingest folder, e.g. ks2. Can be repeated.",
    Arity = ArgumentArity.ZeroOrMore
};

Command listSourcesCommand = new("list-sources", "List the zips in the old pipeline's raw/ and processed/ folders, to choose the manifest from.")
{
    keyVaultOption,
    folderOption
};

listSourcesCommand.SetAction(async (parseResult, cancellationToken) =>
{
    try
    {
        return await AnonymiseCommand.ListSourcesAsync(
            builder.Configuration,
            parseResult.GetValue(keyVaultOption),
            parseResult.GetValue(folderOption) ?? [],
            Console.Out,
            cancellationToken);
    }
    catch (Exception ex) when (ex is not OperationCanceledException)
    {
        await Console.Error.WriteLineAsync(ex.ToString());
        return CompareCommand.ErrorExitCode;
    }
});

Command checkCollisionsCommand = new("check-collisions", "Find the fake keys in the anonymised zips that are already in the test environment.");

checkCollisionsCommand.SetAction(async (parseResult, cancellationToken) =>
{
    try
    {
        return await CheckCollisionsCommand.RunAsync(builder.Configuration, Console.Out, Console.Error, cancellationToken);
    }
    catch (Exception ex) when (ex is not OperationCanceledException)
    {
        await Console.Error.WriteLineAsync(ex.ToString());
        return CompareCommand.ErrorExitCode;
    }
});

RootCommand rootCommand = new("Verifies the new data pipeline against the old one.")
{
    compareCommand,
    anonymiseCommand,
    listSourcesCommand,
    checkCollisionsCommand
};

return await rootCommand.Parse(args).InvokeAsync();

static Dictionary<string, string> ParseRunIds(string[] values) =>
    values
        .Select(v => v.Split('=', 2))
        .ToDictionary(
            p => p.Length == 2 ? p[0] : throw new FormatException($"'{p[0]}' isn't <data file name>=<run ID>."),
            p => p[1]);
