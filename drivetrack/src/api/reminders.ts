import { http } from "./http";
import type {
  Guid,
  Reminder,
  CreateReminderRequest,
  UpdateReminderRequest,
} from "./types";

type ListRemindersParams = {
  fromDue?: string;
  toDue?: string;
  completed?: boolean;
  onlyOverdue?: boolean;
  onlyUpcoming?: boolean;
};

export async function listReminders(
  vehicleId: Guid,
  params?: ListRemindersParams
): Promise<Reminder[]> {
  const { data } = await http.get<Reminder[]>(
    `/vehicles/${vehicleId}/reminders`,
    { params }
  );
  return data;
}

export async function createReminder(
  vehicleId: Guid,
  body: CreateReminderRequest
): Promise<Reminder> {
  const { data } = await http.post<Reminder>(
    `/vehicles/${vehicleId}/reminders`,
    body
  );
  return data;
}

export async function updateReminder(
  reminderId: Guid,
  body: UpdateReminderRequest
): Promise<void> {
  await http.patch(`/reminders/${reminderId}`, body);
}

export async function deleteReminder(reminderId: Guid): Promise<void> {
  await http.delete(`/reminders/${reminderId}`);
}
