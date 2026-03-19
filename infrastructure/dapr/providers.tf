terraform {
  required_version = ">= 1.0"

  backend "azurerm" {
    resource_group_name  = "ay-dev-001"
    storage_account_name = "aydevsa"
    container_name       = "tfstate"
    key                  = "mudakai/dapr.tfstate"
  }

  required_providers {
    helm = {
      source  = "hashicorp/helm"
      version = "~> 2.11"
    }
  }
}

provider "helm" {
  kubernetes {
    config_path    = "~/.kube/config"
    config_context = var.kubernetes_context
  }
}
