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
