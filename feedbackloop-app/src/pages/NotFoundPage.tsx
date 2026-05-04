import { Link } from 'react-router-dom';

export function NotFoundPage() {
  return (
    <main className="grid min-h-screen place-items-center bg-slate-50 p-6">
      <div className="text-center">
        <h1 className="text-4xl font-black">404</h1>
        <p className="mt-2 text-slate-500">Pagina nao encontrada.</p>
        <Link className="mt-4 inline-flex rounded-md bg-brand-600 px-4 py-2 font-bold text-white" to="/">Voltar</Link>
      </div>
    </main>
  );
}
