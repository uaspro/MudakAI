# K8s Helm Deployment Module
# Reusable module for Kubernetes namespace + Helm release pattern

terraform {
  required_providers {
    kubernetes = {
      source  = "hashicorp/kubernetes"
      version = ">= 2.0"
    }
    helm = {
      source  = "hashicorp/helm"
      version = ">= 2.0"
    }
  }
}

# Create namespace if enabled
resource "kubernetes_namespace" "this" {
  count = var.create_namespace ? 1 : 0

  metadata {
    name = var.namespace
    labels = merge(
      {
        "app.kubernetes.io/name"       = var.app_name
        "app.kubernetes.io/managed-by" = "terraform"
      },
      var.namespace_labels
    )
    annotations = var.namespace_annotations
  }
}

# Deploy Helm release
resource "helm_release" "this" {
  name       = var.release_name != "" ? var.release_name : var.app_name
  namespace  = var.create_namespace ? kubernetes_namespace.this[0].metadata[0].name : var.namespace
  chart      = var.helm_chart_path
  repository = var.helm_repository

  # Automatically update dependencies (downloads pi-base-app)
  dependency_update = true

  # Load custom values file if provided
  values = var.helm_values_file != "" && fileexists(var.helm_values_file) ? [
    file(var.helm_values_file)
  ] : var.helm_values

  # Dynamic set blocks for non-sensitive values
  dynamic "set" {
    for_each = merge(
      var.helm_set_values,
      var.force_update ? { deploymentTimestamp = timestamp() } : {}
    )
    content {
      name  = set.key
      value = set.value
    }
  }

  # Dynamic set_sensitive blocks for sensitive values
  dynamic "set_sensitive" {
    for_each = var.helm_set_sensitive_values
    content {
      name  = set_sensitive.key
      value = set_sensitive.value
    }
  }

  # Limit history to prevent buildup
  max_history = var.max_history

  # Timeout and wait settings
  timeout          = var.timeout
  wait             = var.wait
  wait_for_jobs    = var.wait_for_jobs
  create_namespace = false # We handle namespace creation separately

  depends_on = [kubernetes_namespace.this]
}
