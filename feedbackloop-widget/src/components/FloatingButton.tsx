type FloatingButtonProps = {
  onClick: () => void;
};

export function FloatingButton({ onClick }: FloatingButtonProps) {
  return (
    <button className="fl-floating-button" type="button" onClick={onClick}>
      <span className="fl-floating-button__icon" aria-hidden="true">
        ✦
      </span>
      Feedback
    </button>
  );
}
