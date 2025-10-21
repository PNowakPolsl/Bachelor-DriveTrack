import { PieChart, Pie, Cell, Tooltip, Legend, ResponsiveContainer } from "recharts";

export default function ExpensesCategoryChart(){

    const data = [
        { name: "Paliwo", value: 3200},
        { name: "Mechanik", value: 1500},
        { name: "Przegląd", value: 900},
        { name: "Ubezpieczenie", value: 2000},
        { name: "Inne", value: 600},
    ];
    //dane dla pokazu fronta, zmienic jak bedzie back

    const COLORS = ["#3b82f6", "#8b5cf6", "#f59e0b", "#10b981", "#ffd700"];

    return(
        <div className="w-full h-full p-6 bg-white rounded-2xl shadow-md">
            <h2 className="text-xl font-semibold text-gray-800 mb-1">
                Wydatki według kategorii
            </h2>
            <p className="text-gray-500 text-md mb-6">
                Dane z ostatnich 6 miesięcy
            </p>

            <ResponsiveContainer width="100%" height={400}>
                <PieChart>
                    <Pie
                        data={data}
                        cx="50%"
                        cy="50%"
                        labelLine={false}
                        outerRadius={150}
                        fill="#8884d8"
                        dataKey="value"
                    > {data.map((_, index) => (
                            <Cell
                                key={`cell-${index}`}
                                fill={COLORS[index % COLORS.length]}
                            />
                        ))}
                    </Pie>
                    <Tooltip 
                        formatter={(value) => `${value} zł`}
                        contentStyle={{ borderRadius: "10px" }} />
                    <Legend />
                </PieChart>
            </ResponsiveContainer>
        </div>

    );
}