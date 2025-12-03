import './index.css';
import React from "react";
import ReactDOM from "react-dom/client";
import { BrowserRouter, Routes, Route } from "react-router-dom";
import MainMenu from "./pages/MainMenu";
import Registration from "./pages/Registration";
import Login from "./pages/Login";
import Dashboard from "./pages/Dashboard";
import Vehicles from './pages/Vehicles';
import Expenses from './pages/Expenses';
import Reports from './pages/Reports';
import Reminders from './pages/Reminders';
import Profile from './pages/Profile';
import VehicleUsersPage from './pages/VehicleUsersPage';

ReactDOM.createRoot(document.getElementById("root")!).render(
  <React.StrictMode>
    <BrowserRouter basename="/">
      <Routes>
        <Route path="/" element={<MainMenu />} />
        <Route path="/register" element={<Registration />} />
        <Route path="/login" element={<Login />} />
        <Route path="/dashboard" element={<Dashboard />} />
        <Route path="/vehicles" element={<Vehicles />} />
        <Route path="/expenses" element={<Expenses />} />
        <Route path="/reports" element={<Reports />} />
        <Route path="/reminders" element={<Reminders />} />
        <Route path="/profile" element={<Profile />} />
        <Route path="/vehicles/:id/users" element={<VehicleUsersPage />} />
      </Routes>
    </BrowserRouter>
  </React.StrictMode>
);

