terraform {
  required_version = ">= 1.7"

  backend "azurerm" {
    resource_group_name  = "ay-dev-001"
    storage_account_name = "aydevsa"
    container_name       = "tfstate"
    key                  = "mudakai/terraform.tfstate"
  }

  required_providers {
    kubernetes = {
      source  = "hashicorp/kubernetes"
      version = "~> 2.23"
    }
    helm = {
      source  = "hashicorp/helm"
      version = "~> 2.11"
    }
  }
}

provider "kubernetes" {
  config_path    = "~/.kube/config"
  config_context = var.kubernetes_context
}

provider "helm" {
  kubernetes {
    config_path    = "~/.kube/config"
    config_context = var.kubernetes_context
  }
}
