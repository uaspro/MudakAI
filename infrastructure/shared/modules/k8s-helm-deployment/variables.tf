variable "namespace" {
  description = "Kubernetes namespace for the deployment"
  type        = string
}

variable "app_name" {
  description = "Application name used for labels and default release name"
  type        = string
}

variable "release_name" {
  description = "Helm release name (defaults to app_name if not specified)"
  type        = string
  default     = ""
}

variable "helm_chart_path" {
  description = "Path to local Helm chart (use this OR helm_repository)"
  type        = string
  default     = ""
}

variable "helm_repository" {
  description = "Helm chart repository URL (use this OR helm_chart_path)"
  type        = string
  default     = ""
}

variable "helm_values_file" {
  description = "Path to custom Helm values file"
  type        = string
  default     = ""
}

variable "helm_values" {
  description = "List of Helm values as YAML strings"
  type        = list(string)
  default     = []
}

variable "helm_set_values" {
  description = "Map of Helm values to set (non-sensitive)"
  type        = map(string)
  default     = {}
}

variable "helm_set_sensitive_values" {
  description = "Map of Helm values to set (sensitive)"
  type        = map(string)
  default     = {}
  sensitive   = true
}

variable "create_namespace" {
  description = "Whether to create the namespace"
  type        = bool
  default     = true
}

variable "namespace_labels" {
  description = "Additional labels for the namespace"
  type        = map(string)
  default     = {}
}

variable "namespace_annotations" {
  description = "Annotations for the namespace"
  type        = map(string)
  default     = {}
}

variable "max_history" {
  description = "Maximum number of Helm release revisions to keep"
  type        = number
  default     = 5
}

variable "force_update" {
  description = "Add deployment timestamp to force updates"
  type        = bool
  default     = true
}

variable "timeout" {
  description = "Timeout for Helm operations in seconds"
  type        = number
  default     = 300
}

variable "wait" {
  description = "Wait for resources to be ready"
  type        = bool
  default     = true
}

variable "wait_for_jobs" {
  description = "Wait for jobs to complete"
  type        = bool
  default     = false
}
