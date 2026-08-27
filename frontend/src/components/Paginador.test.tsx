import { fireEvent, render, screen } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import { Paginador } from '../pages';

describe('Paginador', () => {
  it('exibe totais e permite avançar e retornar páginas', () => {
    const mudarPagina = vi.fn();
    render(<Paginador paginaAtual={2} totalPaginas={4} totalItens={75} mudarPagina={mudarPagina} />);

    expect(screen.getByText('Página 2 de 4 — 75 registro(s)')).toBeInTheDocument();
    fireEvent.click(screen.getByRole('button', { name: 'Anterior' }));
    fireEvent.click(screen.getByRole('button', { name: 'Próxima' }));
    expect(mudarPagina).toHaveBeenNthCalledWith(1, 1);
    expect(mudarPagina).toHaveBeenNthCalledWith(2, 3);
  });

  it('desabilita navegação nos limites', () => {
    render(<Paginador paginaAtual={1} totalPaginas={2} totalItens={21} mudarPagina={() => undefined} />);
    expect(screen.getByRole('button', { name: 'Anterior' })).toBeDisabled();
    expect(screen.getByRole('button', { name: 'Próxima' })).toBeEnabled();
  });
});
