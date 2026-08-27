import { expect, test } from '@playwright/test';

test('tela de login possui os controles essenciais', async ({ page }) => {
  await page.goto('/login');
  await expect(page.getByRole('heading', { name: 'Gestão Comercial' })).toBeVisible();
  await expect(page.getByLabel('Email')).toBeVisible();
  await expect(page.getByLabel('Senha')).toBeVisible();
  await expect(page.getByRole('button', { name: 'Entrar' })).toBeVisible();
});
