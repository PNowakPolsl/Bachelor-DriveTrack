import { http } from './http';
import type { CreateFuelEntryRequest, Guid } from './types';

export async function createFuelEntry(
  vehicleId: Guid,
  input: CreateFuelEntryRequest
) {
  const { data } = await http.post(`/vehicles/${vehicleId}/fuel-entries`, input);
  return data;
}
