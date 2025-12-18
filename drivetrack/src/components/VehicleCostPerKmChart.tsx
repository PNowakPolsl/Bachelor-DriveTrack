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
  data: { vehicle: string; costPer100Km: number }[];
  rangeLabel: string;
};

export default function VehicleCostPerKmChart({ data, rangeLabel }: Props) {
  return (
    <div className="w-full h-full p-6 bg-white rounded-2xl shadow-md">
      <h2 className="text-xl font-semibold text-gray-800 mb-1">
        Koszt na 100 km wg pojazdu
      </h2>
      <p className="text-gray-500 text-md mb-6">
        Wszystkie koszty / dystans w {rangeLabel}
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
                })} zł / 100 km`,
                "Koszt na 100km",
            ]} />
            <Bar
              dataKey="costPer100Km"
              fill="#f97316"
              radius={[8, 8, 0, 0]}
            />
          </BarChart>
        </ResponsiveContainer>
      </div>
    </div>
  );
}
