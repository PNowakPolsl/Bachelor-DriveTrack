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
  baseOdometerKm?: number | null;
}

export type Category = {
  id: Guid;
  name: string;
  ownerUserId?: Guid | null;
  createdAt: string;
};

export type Expense = {
  id: Guid;
  date: string;
  amount: number;
  description?: string | null;
  odometerKm?: number | null;
  category: { categoryId: Guid;
              name: string;
  };
};

export type CreateExpenseRequest = {
  categoryId: Guid;
  date: string;
  amount: number;
  description?: string | null;
  odometerKm?: number | null;
};

export interface FuelType {
  id: Guid;
  name: string;
  defaultUnit: string;
}

export interface CreateFuelEntryRequest {
  fuelTypeId: Guid;
  unit?: string | null;
  date: string;
  volume: number;
  pricePerUnit: number;
  odometerKm: number;
  isFullTank?: boolean;
  station?: string | null;
}