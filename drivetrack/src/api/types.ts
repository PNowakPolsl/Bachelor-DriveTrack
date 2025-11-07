export type Guid = string;

export interface Vehicle {
  id: Guid;
  name: string;
  make: string;
  model: string;
  plate?: string | null;
  year?: number | null;
  createdAt?: string;
}

export interface VehicleFuelType {
  id: Guid;
  name: string;
  defaultUnit: string;
}

export interface VehicleDetails extends Vehicle {
  fuelTypes: VehicleFuelType[];
}

export interface CreateVehicleRequest {
  name: string;
  make: string;
  model: string;
  plate?: string | null;
  year?: number | null;
}
