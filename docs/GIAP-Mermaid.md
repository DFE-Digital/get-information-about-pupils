::: mermaid

graph TD
X[DFE Sign-in] --> A
A --> X
A[GIAP Web] --- B[GIAP Azure Function]
B --- E[Azure BLOB storage]
B --- D[Azure Cognitive Search]
B --- C[Azure CosmosDB]
D --- C

:::