import { http } from './http';
import type { CreateFuelEntryRequest, Guid } from './types';

export async function createFuelEntry(
  vehicleId: Guid,
  input: CreateFuelEntryRequest
) {
  const { data } = await http.post(`/vehicles/${vehicleId}/fuel-entries`, input);
  return data;
}

export async function listStations(vehicleId: Guid): Promise<string[]> {
  const { data } = await http.get<string[]>(`/vehicles/${vehicleId}/stations`);
  return data;
}