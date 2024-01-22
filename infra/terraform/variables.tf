variable "hcloud_token" {
  type = string
  sensitive = true
}

variable "ssh_key_fingerprint" {
  type = string
  default = "d7:d6:8f:54:a0:ac:43:2e:83:77:7d:90:fd:a1:3a:84"
}