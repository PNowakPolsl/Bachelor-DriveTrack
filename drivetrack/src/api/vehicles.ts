import { http } from './http';
import type { Vehicle, VehicleDetails, CreateVehicleRequest, Guid } from './types';

export async function listVehicles(): Promise<Vehicle[]> {
    const { data } = await http.get<Vehicle[]>('/vehicles');
    return data;
}

export async function createVehicle(input: CreateVehicleRequest): Promise<Vehicle> {
    const { data } = await http.post<Vehicle>('/vehicles', input);
    return data;
}

export async function getVehicle(id: Guid): Promise<VehicleDetails> {
  const { data } = await http.get<VehicleDetails>(`/vehicles/${id}`);
  return data;
}

export async function assignFuelType(vehicleId: Guid, fuelTypeId: Guid): Promise<void> {
  await http.post(`/vehicles/${vehicleId}/fuel-types`, { fuelTypeId });
}

export type FuelTypeDict = { id: string; name: string; defaultUnit: string };
export async function listFuelTypes(): Promise<FuelTypeDict[]> {
  const { data } = await http.get<FuelTypeDict[]>('/fuel-types');
  return data;
}