resource "hcloud_network" "network" {
  name     = "main_network"
  ip_range = "10.0.0.0/16"
}

resource "hcloud_network_subnet" "subnet" {
  type         = "cloud"
  network_id   = hcloud_network.network.id
  network_zone = "eu-central"
  ip_range     = "10.0.1.0/24"
}

module "api_server_1" {
  source = "./modules/api-server"

  name = "api1"
  location = "hel1"
  server_type = "cx21"
  image = "ubuntu-22.04"
  network_id = hcloud_network.network.id
}
