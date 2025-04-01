import React, { useState } from "react";
import "./AdminPageAuth.css";
import { useNavigate } from "react-router-dom";
import { AuthService } from "../../shared/services/AuthService";
import { AuthModel } from "../../shared/models/AuthModel"; 
import NSTUlogo from '../../assets/pics/NSTU-logo.png';

const AdminPageAuth: React.FC<{ onLogin?: () => void }> = ({ onLogin }) => {
  const [login, setLogin] = useState("");
  const [password, setPassword] = useState("");
  const navigate = useNavigate();

  const handleLogin = async () => {

    const authData: AuthModel = { login, password };
    try {
      const result = await AuthService.Auth(authData);
      
      if (result.success) {
        onLogin?.();
        navigate('/admin/dashboard', { 
          state: { 
            directions: result.data,
            status: 200 
          } 
        });
      } else {
        alert(result.error || "Ошибка авторизации");
      }
    } catch (error) {
        console.error("Неожиданная ошибка:", error);
        alert("Произошла непредвиденная ошибка");
    }
  };

  return (
    <div className="admin-auth-container">
      <img src={NSTUlogo} alt="logo" className="logo" />
      <h1 className="auth-title">Вход в админ-панель</h1>
      <p className="auth-subtitle">Пожалуйста, введите логин и пароль</p>

      <input
        type="text"
        placeholder="Логин"
        value={login}
        onChange={(e) => setLogin(e.target.value)}
        className="auth-input"
      />
      <input
        type="password"
        placeholder="Пароль"
        value={password}
        onChange={(e) => setPassword(e.target.value)}
        className="auth-input"
      />
      <button onClick={handleLogin} className="auth-button">
        Войти
      </button>
    </div>
  );
};

export default AdminPageAuth;