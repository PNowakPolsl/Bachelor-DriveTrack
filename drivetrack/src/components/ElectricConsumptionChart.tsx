import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  Tooltip,
  CartesianGrid,
  ResponsiveContainer,
} from "recharts";

type Props = {
  data: { month: string; consumption: number }[];
  rangeLabel: string;
};

export default function ElectricConsumptionChart({ data, rangeLabel }: Props) {
  return (
    <div className="w-full h-full p-6 bg-white rounded-2xl shadow-md">
      <h2 className="text-xl font-semibold text-gray-800 mb-1">
        Średnie zużycie energii kWh/100km
      </h2>
      <p className="text-gray-500 text-md mb-6">
        Dane z {rangeLabel} (EV – jednostka kWh)
      </p>

      <ResponsiveContainer width="100%" height={300}>
        <LineChart data={data}>
          <CartesianGrid strokeDasharray="3 3" stroke="#e5e7eb" />
          <XAxis dataKey="month" tick={{ fill: "#6b7280" }} />
          <YAxis tick={{ fill: "#6b7280" }} />
          <Tooltip formatter={(value) => `${value} kWh/100km`} />
          <Line
            type="monotone"
            dataKey="consumption"
            stroke="#10b981"
            strokeWidth={3}
            dot={{ r: 5, fill: "#10b981" }}
          />
        </LineChart>
      </ResponsiveContainer>
    </div>
  );
}
