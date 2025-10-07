import { useState } from 'react'
import reactLogo from './assets/react.svg'
import viteLogo from '/vite.svg'
import './index.css' // upewnij się, że index.css zawiera @tailwind base/components/utilities

function App() {
  const [count, setCount] = useState(0)

  return (
    <div className="flex flex-col items-center justify-center min-h-screen bg-gray-900 text-white">
      <div className="flex space-x-4 mb-8">
        <a href="https://vite.dev" target="_blank">
          <img src={viteLogo} className="h-20 w-20" alt="Vite logo" />
        </a>
        <a href="https://react.dev" target="_blank">
          <img src={reactLogo} className="h-20 w-20" alt="React logo" />
        </a>
      </div>

      <h1 className="text-5xl font-bold mb-8">Vite + React + Tailwind</h1>

      <div className="flex flex-col items-center bg-gray-800 p-6 rounded-lg shadow-lg">
        <button
          className="px-4 py-2 bg-blue-600 hover:bg-blue-700 rounded text-white font-semibold mb-4"
          onClick={() => setCount((count) => count + 1)}
        >
          count is {count}
        </button>
        <p>
          Edit <code>src/App.tsx</code> and save to test HMR
        </p>
      </div>

      <p className="mt-8 text-gray-400">
        Click on the Vite and React logos to learn more
      </p>
    </div>
  )
}

export default App
