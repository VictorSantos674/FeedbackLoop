export function App() {
  return (
    <main className="app-shell">
      <aside className="sidebar">
        <strong>FeedbackLoop</strong>
        <nav>
          <a href="/">Boards</a>
          <a href="/">Roadmap</a>
          <a href="/">Settings</a>
        </nav>
      </aside>
      <section className="content">
        <header>
          <span>Admin Panel</span>
          <h1>Boards</h1>
        </header>
        <div className="empty-state">
          <h2>Modelo inicial pronto</h2>
          <p>Os próximos passos vão conectar autenticação, boards, posts e roadmap à API.</p>
        </div>
      </section>
    </main>
  );
}
