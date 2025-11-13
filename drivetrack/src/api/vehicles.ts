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

export async function getVehicleOdometer(
  id: Guid
): Promise<{ odometerKm: number | null; source: string | null; date: string | null }> {
  try {
    const { data } = await http.get(`/vehicles/${id}/odometer`);
    return {
      odometerKm: data.odometerKm ?? null,
      source: data.source ?? null,
      date: data.date ?? null,
    };
  } catch (err: any) {
    console.warn("getVehicleOdometer error", err?.response?.status, err?.message);
    return { odometerKm: null, source: null, date: null };
  }
}

export async function deleteVehicle(id: Guid): Promise<void> {
  await http.delete(`/vehicles/${id}`);
}

export async function updateVehicle(id: Guid, input: CreateVehicleRequest): Promise<void> {
  await http.put(`/vehicles/${id}`, input);
}

export async function unassignFuelType(vehicleId: Guid, fuelTypeId: Guid): Promise<void> {
  await http.delete(`/vehicles/${vehicleId}/fuel-types/${fuelTypeId}`);
}

