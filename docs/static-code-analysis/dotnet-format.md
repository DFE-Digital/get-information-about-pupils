# Dotnet format

The solution uses a [`.editorconfig`](../../editorconfig) as a way of enforcing code style which has cross-platform-editor support.

You can use [dotnet format](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-format) to automatically apply/verify the code styles specified.

It is part of the `dotnet sdk` so does not require separated install or versioning

```sh
# in the same folder as the .sln
dotnet format
```

In our pipeline we use `--verify-no-changes` to proof no formatting is required to any files

```sh
dotnet format --verbosity detailed --include DfE.GIAP.Core/ DfE.GIAP.Core.UnitTests/ --verify-no-changes  # will produce non-0 exit code if files require formatting
echo $? #print exit code
```
