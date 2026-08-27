import axios, { AxiosError } from 'axios';
export interface Problema {
  title?: string;
  detail?: string;
  status?: number;
}
export const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL ?? 'http://localhost:5291/api',
  withCredentials: true,
  headers: { 'Content-Type': 'application/json' },
});
api.interceptors.response.use(
  (resposta) => resposta,
  (erro) => {
    if (erro instanceof AxiosError && erro.response?.status === 401) {
      window.dispatchEvent(new Event('auth:unauthorized'));
    }
    return Promise.reject(erro);
  },
);
export function mensagemErro(erro: unknown): string {
  if (erro instanceof AxiosError) {
    const problema = erro.response?.data as Problema | undefined;
    return problema?.detail ?? problema?.title ?? 'Falha ao comunicar com o servidor.';
  }
  return erro instanceof Error ? erro.message : 'Erro inesperado.';
}
