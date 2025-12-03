import { http } from "./http";
import axios from "axios";

export type AuthResponse = {
  userId: string;
  email: string;
  name: string;
  token: string;
};

type RegisterBody = {
  email: string;
  password: string;
  name: string;
};

type LoginBody = {
  email: string;
  password: string;
};

type ChangePasswordBody = {
  currentPassword: string;
  newPassword: string;
};

const API_BASE_URL = "http://localhost:5018";

const STORAGE_KEY = "dt_auth";


export async function register(body: RegisterBody): Promise<AuthResponse> {
  const { data } = await http.post<AuthResponse>("/auth/register", body);
  return data;
}

export async function login(body: LoginBody): Promise<AuthResponse> {
  const { data } = await http.post<AuthResponse>("/auth/login", body);
  return data;
}


export function saveAuth(auth: AuthResponse) {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(auth));
}

export function getAuth(): AuthResponse | null {
  const raw = localStorage.getItem(STORAGE_KEY);
  if (!raw) return null;
  try {
    return JSON.parse(raw) as AuthResponse;
  } catch {
    return null;
  }
}

export function logout() {
  localStorage.removeItem(STORAGE_KEY);
}

// pomocniczo – tylko „użyteczny” user bez tokena
export function getCurrentUser() {
  const auth = getAuth();
  if (!auth) return null;
  return {
    id: auth.userId,
    email: auth.email,
    name: auth.name,
  };
}

export async function changePassword(body: ChangePasswordBody): Promise<void> {
  const auth = getAuth();
  if (!auth) {
    throw new Error("Użytkownik nie jest zalogowany.");
  }

  await axios.post(`${API_BASE_URL}/auth/change-password`, body, {
    headers: {
      Authorization: `Bearer ${auth.token}`,
    },
  });
}
