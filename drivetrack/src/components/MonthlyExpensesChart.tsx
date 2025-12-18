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
  data: { month: string; amount: number }[];
  rangeLabel: string;
};

export default function MonthlyExpensesChart({ data, rangeLabel }: Props) {
  return (
    <div className="bg-white rounded-2xl shadow-md pt-6 mt-10">
      <h2 className="text-2xl font-semibold text-gray-800 px-6">
        Miesięczne wydatki
      </h2>
        <p className="text-gray-500 text-md mt-1 px-6">
          Twoje wydatki w ciągu ostatnich 6 miesięcy
        </p>
      <div className="mt-6 h-80">
        <ResponsiveContainer width="100%" height="100%">
          <BarChart data={data}>
            <CartesianGrid strokeDasharray="3 3" stroke="#cbcdd1" />
            <XAxis dataKey="month" stroke="#6b7280" />
            <YAxis stroke="#6b7280" />
            <Tooltip formatter={(value) => `${value} zł`} />
            <Bar
              dataKey="amount"
              fill="#3b82f6"
              radius={[10, 10, 0, 0]}
            />
          </BarChart>
        </ResponsiveContainer>
      </div>
    </div>
  );
}
