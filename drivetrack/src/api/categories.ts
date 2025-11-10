import { http } from './http';
import type { Category, Guid } from './types';

export async function listCategories(ownerUserId?: Guid): Promise<Category[]> {
    const { data } = await http.get<Category[]>('/categories', {
        params: ownerUserId ? { ownerUserId } : undefined,
    });
    return data;
}

export async function createCategory(name: string, ownerUserId?: Guid | null) {
    const payload = { name, ownerUserId: ownerUserId ?? null };
    const { data } = await http.post('/categories', payload);
    return data as Category;
}
