'use client';
import { useState } from 'react';
import axios from 'axios';

// Configuração Base: Use o endereço da sua API
const API_BASE_URL = 'http://localhost:5196'; 

export default function LoginPage() {
  const [email, setEmail] = useState('');
  const [senha, setSenha] = useState('');
  const [message, setMessage] = useState('');

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    setMessage('Tentando logar...');

    try {
      const response = await axios.post(`${API_BASE_URL}/login`, { email, senha });
      
      // IDEAL: O token seria salvo em um HttpOnly Cookie aqui
      // Por enquanto, apenas exibimos uma mensagem de sucesso
      const token = response.data.token;
      setMessage(`Login bem-sucedido! Token obtido: ${token.substring(0, 20)}...`);

      // Aqui você faria o redirecionamento para o dashboard
      // window.location.href = '/veiculos';

    } catch (error) {
      if (axios.isAxiosError(error) && error.response) {
        if (error.response.status === 401) {
             setMessage('Erro: Credenciais inválidas.');
        } else {
             setMessage(`Erro: ${error.response.data.error || 'Falha na comunicação com a API.'}`);
        }
      } else {
        setMessage('Erro de rede ou interno.');
      }
    }
  };

  return (
    <div className="flex items-center justify-center min-h-screen bg-gray-100">
      <form onSubmit={handleLogin} className="p-6 bg-white rounded shadow-md w-96">
        <h2 className="text-2xl font-bold mb-4">Acesso NexoPark</h2>
        
        <div className="mb-4">
          <label className="block text-sm font-medium text-gray-700">Email:</label>
          <input
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            className="mt-1 p-2 w-full border rounded-md"
            required
          />
        </div>
        
        <div className="mb-4">
          <label className="block text-sm font-medium text-gray-700">Senha:</label>
          <input
            type="password"
            value={senha}
            onChange={(e) => setSenha(e.target.value)}
            className="mt-1 p-2 w-full border rounded-md"
            required
          />
        </div>
        
        <button
          type="submit"
          className="w-full bg-blue-500 text-white p-2 rounded-md hover:bg-blue-600"
        >
          Login
        </button>
        
        {message && <p className="mt-4 text-center text-sm font-medium">{message}</p>}
      </form>
    </div>
  );
}