import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  Tooltip,
  ResponsiveContainer,
  CartesianGrid,
} from "recharts";

type Props = {
  data: { vehicle: string; amount: number }[];
  rangeLabel: string;
};

export default function VehicleExpensesChart({ data, rangeLabel }: Props) {
  return (
    <div className="w-full h-full p-6 bg-white rounded-2xl shadow-md">
      <h2 className="text-xl font-semibold text-gray-800 mb-1">
        Wydatki według pojazdu
      </h2>
      <p className="text-gray-500 text-md mb-6">
        Łączne koszty z {rangeLabel}
      </p>

      <div className="h-72">
        <ResponsiveContainer width="100%" height="100%">
          <BarChart data={data}>
            <CartesianGrid strokeDasharray="3 3" stroke="#e5e7eb" />
            <XAxis dataKey="vehicle" tick={{ fill: "#6b7280" }} />
            <YAxis tick={{ fill: "#6b7280" }} />
            <Tooltip formatter={(value) => [
                `${value.toLocaleString("pl-PL", {
                minimumFractionDigits: 2,
                maximumFractionDigits: 2,
                })} zł`,
                "Koszt",
            ]} />
            <Bar dataKey="amount" fill="#6366f1" radius={[8, 8, 0, 0]} />
          </BarChart>
        </ResponsiveContainer>
      </div>
    </div>
  );
}
