// src/api/dashboard.ts
const API_URL = "http://localhost:5018";

export type UpcomingReminder = {
  id: string;
  title: string;
  dueDate: string;
  vehicleName: string;
  daysLeft: number;
};

export async function getUpcomingReminders(): Promise<UpcomingReminder[]> {
  const token = localStorage.getItem("token") ?? "";

  const res = await fetch(`${API_URL}/dashboard/upcoming-reminders`, {
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${token}`,
    },
  });

  if (!res.ok) {
    const text = await res.text();
    throw new Error(text || "Błąd pobierania przypomnień");
  }

  return res.json();
}
