import type React from 'react';
import type { ButtonHTMLAttributes, InputHTMLAttributes, PropsWithChildren } from 'react';
import { clsx } from 'clsx';
export function Button(propriedades: ButtonHTMLAttributes<HTMLButtonElement>) {
  return <button {...propriedades} className={clsx('btn', propriedades.className)} />;
}
export function Input(propriedades: InputHTMLAttributes<HTMLInputElement>) {
  return <input {...propriedades} />;
}
export function Card({ children, className }: { children: React.ReactNode; className?: string }) {
  return <section className={clsx('panel', className)}>{children}</section>;
}
export function Empty({ children }: PropsWithChildren) {
  return <div className="py-10 text-center text-slate-500">{children}</div>;
}
export function Loading() {
  return <div className="py-10 text-center text-slate-500">Carregando…</div>;
}
