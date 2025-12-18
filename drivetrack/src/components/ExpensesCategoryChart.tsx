import {
  PieChart,
  Pie,
  Cell,
  Tooltip,
  Legend,
  ResponsiveContainer,
} from "recharts";

type Props = {
  data: { name: string; value: number }[];
  rangeLabel: string;
};

const COLORS = ["#3be3f6", "#8b5cf6", "#f59e0b", "#10b981", "#ffd700"];

const renderLabel = (entry: any) => {
  const percent = entry.percent * 100;
  return `${entry.name}: ${percent.toFixed(0)}%`;
};

export default function ExpensesCategoryChart({ data, rangeLabel }: Props) {
  const sorted = [...data].sort((a, b) => b.value - a.value);

  return (
    <div className="w-full h-full p-6 bg-white rounded-2xl shadow-md">
      <h2 className="text-xl font-semibold text-gray-800 mb-1">
        Wydatki według kategorii
      </h2>
      <p className="text-gray-500 text-md mb-6">
        Dane z {rangeLabel}
      </p>

      {/* WYKRES */}
      <ResponsiveContainer width="100%" height={360}>
        <PieChart>
          <Pie
            data={data}
            cx="50%"
            cy="50%"
            labelLine={false}
            outerRadius={130}
            fill="#8884d8"
            dataKey="value"
            label={renderLabel}
          >
            {data.map((_, index) => (
              <Cell
                key={`cell-${index}`}
                fill={COLORS[index % COLORS.length]}
              />
            ))}
          </Pie>

          <Tooltip
            formatter={(value: number) => `${value} zł`}
            contentStyle={{ borderRadius: "10px" }}
          />

          <Legend />
        </PieChart>
      </ResponsiveContainer>

      {/* LISTA POD WYKRESEM */}
      <div className="mt-6 border-t pt-4">
        <h3 className="text-md font-semibold text-gray-700 mb-3">
          Szczegóły kategorii:
        </h3>

        <ul className="space-y-2">
          {sorted.map((item, index) => (
            <li
              key={item.name}
              className="flex items-center justify-between text-gray-800"
            >
              <div className="flex items-center gap-2">
                <span
                  className="w-3 h-3 rounded-full inline-block"
                  style={{ backgroundColor: COLORS[index % COLORS.length] }}
                />
                {item.name}
              </div>

              <span className="font-semibold">{item.value} zł</span>
            </li>
          ))}
        </ul>
      </div>
    </div>
  );
}
