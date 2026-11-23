data "azurerm_resource_group" "app" {
  name = "${var.azure_resource_prefix}-${var.service_short}-${var.config_short}-rg"
}

# Holds the files uploaded for the Worker's file ingestion job
resource "azurerm_storage_account" "ingest" {
  name                              = "${var.azure_resource_prefix}${var.service_short}${var.config_short}ingestsa"
  location                          = data.azurerm_resource_group.app.location
  resource_group_name               = data.azurerm_resource_group.app.name
  account_replication_type          = var.environment != "production" ? "LRS" : "GRS"
  account_tier                      = "Standard"
  account_kind                      = "StorageV2"
  min_tls_version                   = "TLS1_2"
  infrastructure_encryption_enabled = true
  allow_nested_items_to_be_public   = false

  lifecycle {
    ignore_changes = [
      tags
    ]
  }
}

resource "azurerm_storage_container" "ingest" {
  name                  = "ingest"
  storage_account_id    = azurerm_storage_account.ingest.id
  container_access_type = "private"
}

# Holds the metadata documents published from ingested zips
resource "azurerm_storage_container" "metadata" {
  name                  = "metadata"
  storage_account_id    = azurerm_storage_account.ingest.id
  container_access_type = "private"
}

# Holds the prepared downloads written from ingested data files
resource "azurerm_storage_container" "downloads" {
  name                  = "downloads"
  storage_account_id    = azurerm_storage_account.ingest.id
  container_access_type = "private"
}

# Holds the verification tool's reports, and the anonymised files it's run against in test
resource "azurerm_storage_container" "verification" {
  name                  = "verification"
  storage_account_id    = azurerm_storage_account.ingest.id
  container_access_type = "private"
}
