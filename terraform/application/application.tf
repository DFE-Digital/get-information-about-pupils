module "application_configuration" {
  source = "./vendor/modules/aks//aks/application_configuration"

  namespace              = var.namespace
  environment            = var.environment
  azure_resource_prefix  = var.azure_resource_prefix
  service_short          = var.service_short
  config_short           = var.config_short
  secret_key_vault_short = "app"
  config_variables_path  = "${path.module}/config/${var.config}.yml"

  config_variables = {
    ENVIRONMENT_NAME = var.environment
    PGSSLMODE        = local.postgres_ssl_mode
  }
  secret_variables = {
    ConnectionStrings__GiapDb = module.postgres.dotnet_connection_string
  }
}

module "web_application" {
  source = "./vendor/modules/aks//aks/application"

  depends_on = [module.migrations]

  is_web = true

  namespace    = var.namespace
  environment  = var.environment
  service_name = var.service_name

  cluster_configuration_map  = module.cluster_data.configuration_map
  kubernetes_config_map_name = module.application_configuration.kubernetes_config_map_name
  kubernetes_secret_name     = module.application_configuration.kubernetes_secret_name

  probe_path = var.probe_path

  docker_image = var.docker_image
  command      = ["/bin/ash", "-c", "cd /apps/web/; exec dotnet DfE.GIAP.Web.dll"]
  enable_logit = true

  send_traffic_to_maintenance_page = var.send_traffic_to_maintenance_page
}

module "worker_application" {
  source = "./vendor/modules/aks//aks/application"

  depends_on = [module.migrations]

  name   = "worker"
  is_web = false

  namespace    = var.namespace
  environment  = var.environment
  service_name = var.service_name

  cluster_configuration_map  = module.cluster_data.configuration_map
  kubernetes_config_map_name = module.application_configuration.kubernetes_config_map_name
  kubernetes_secret_name     = module.application_configuration.kubernetes_secret_name

  docker_image = var.docker_image
  command      = ["/bin/ash", "-c", "cd /apps/worker/; exec dotnet DfE.GIAP.Worker.dll"]
  replicas     = 1
  enable_logit = true
}
