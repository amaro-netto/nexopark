'use client';
import { useState } from 'react';
import axios from 'axios';

const API_BASE_URL = '/api'; 

interface NovoVeiculoProps {
  onSuccess: () => void;
  token: string;
}

export default function NovoVeiculo({ onSuccess, token }: NovoVeiculoProps) {
  const [placa, setPlaca] = useState('');
  const [modelo, setModelo] = useState('');
  const [cor, setCor] = useState('');
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setMessage('Registrando veículo...');
    setError('');

    try {
      // 1. Envia a requisição POST com o token de autorização
      await axios.post(`${API_BASE_URL}/veiculos`, 
        { placa, modelo, cor }, 
        {
          headers: {
            Authorization: `Bearer ${token}`,
          },
        }
      );
      
      setMessage('Veículo registrado com sucesso!');
      setPlaca('');
      setModelo('');
      setCor('');
      
      // 2. Chama a função de sucesso para recarregar a lista na página pai
      onSuccess(); 

    } catch (err) {
      if (axios.isAxiosError(err) && err.response) {
        // Erro 409: Conflito (Placa duplicada)
        if (err.response.status === 409) {
          setError(`Erro 409: Placa ${placa} já está registrada!`);
        } else {
          setError(`Erro ${err.response.status}: Falha ao registrar.`);
        }
      } else {
        setError('Erro de rede ou proxy.');
      }
    }
  };

  return (
    <div className="mb-8 p-6 bg-white shadow-lg rounded-lg">
      <h2 className="text-xl font-bold mb-4">Registrar Novo Veículo</h2>
      <form onSubmit={handleSubmit} className="space-y-4">
        <div className="grid grid-cols-3 gap-4">
          <input
            type="text"
            placeholder="Placa (ex: ABC1234)"
            value={placa}
            onChange={(e) => setPlaca(e.target.value.toUpperCase())}
            className="p-2 border rounded"
            required
            maxLength={7}
          />
          <input
            type="text"
            placeholder="Modelo"
            value={modelo}
            onChange={(e) => setModelo(e.target.value)}
            className="p-2 border rounded"
            required
          />
          <input
            type="text"
            placeholder="Cor"
            value={cor}
            onChange={(e) => setCor(e.target.value)}
            className="p-2 border rounded"
            required
          />
        </div>
        <button
          type="submit"
          className="w-full bg-green-600 text-white p-2 rounded hover:bg-green-700 transition"
        >
          Registrar
        </button>
      </form>
      {error && <p className="mt-3 text-sm text-red-500 font-medium">{error}</p>}
      {message && !error && <p className="mt-3 text-sm text-green-600 font-medium">{message}</p>}
    </div>
  );
}