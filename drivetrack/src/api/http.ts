import axios from "axios";

const baseURL = import.meta.env.VITE_API_URL ?? "http://localhost:5018";

const STORAGE_KEY = "dt_auth";

export const http = axios.create({ baseURL });

http.interceptors.request.use((config: any) => {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (raw) {
      const auth = JSON.parse(raw);

      if (auth?.token) {
        if (!config.headers) {
          config.headers = {};
        }

        config.headers.Authorization = `Bearer ${auth.token}`;
      }
    }
  } catch {
  }

  return config;
});

http.interceptors.response.use(
  (res) => res,
  (err) => {
    if (err?.response?.status === 401) {
      localStorage.removeItem(STORAGE_KEY);

      if (window.location.pathname !== "/login") {
        window.location.href = "/login";
      }
    }

    return Promise.reject(err);
  }
);
