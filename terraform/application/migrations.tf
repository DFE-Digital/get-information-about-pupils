module "migrations_job_configuration" {
  source = "./vendor/modules/aks//aks/application_configuration"

  namespace              = var.namespace
  environment            = var.environment
  azure_resource_prefix  = var.azure_resource_prefix
  service_short          = var.service_short
  config_short           = var.config_short
  secret_key_vault_short = "inf"

  config_variables = {
    ENVIRONMENT_NAME = var.environment
  }
  secret_variables = {
    # Disable the command timeout so long-running migrations aren't cancelled
    CONNECTION_STRING = "${module.postgres.dotnet_connection_string};Command Timeout=0"
  }
}

module "migrations" {
  source = "./vendor/modules/aks//aks/job_configuration"

  namespace    = var.namespace
  environment  = var.environment
  service_name = var.service_name
  docker_image = var.docker_image
  commands     = ["/app/efbundle"]
  arguments    = ["--connection", "$(CONNECTION_STRING)"]
  job_name     = "migrations"
  enable_logit = var.enable_logit

  config_map_ref = module.migrations_job_configuration.kubernetes_config_map_name
  secret_ref     = module.migrations_job_configuration.kubernetes_secret_name
  cpu            = module.cluster_data.configuration_map.cpu_min
}
