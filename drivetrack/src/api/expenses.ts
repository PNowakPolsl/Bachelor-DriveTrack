import { http } from './http';
import type { Expense, CreateExpenseRequest, Guid } from './types';

export async function listExpenses(
    vehicleId: Guid,
    params?: { from?: string; to?: string; categoryId?: Guid }
):  Promise<Expense[]> {
        const { data } = await http.get(`/vehicles/${vehicleId}/expenses`, { params });
        return data;
}

export async function createExpense(
    vehicleId: Guid,
    input: CreateExpenseRequest
):  Promise<Expense> {
        const { data } = await http.post(`/vehicles/${vehicleId}/expenses`, input);
        return data;
}
