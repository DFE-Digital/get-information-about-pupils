module "domains_infrastructure" {
  source                 = "./vendor/modules/domains//domains/infrastructure"
  hosted_zone            = var.hosted_zone
  deploy_default_records = var.deploy_default_records
}

module "dns_records" {
  source      = "./vendor/modules/domains//dns/records"
  hosted_zone = var.hosted_zone
}