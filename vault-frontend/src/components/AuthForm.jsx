
import { useState } from "react";

export default function AuthForm({ title, submitText, onSubmit, isLoading=false }) {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [touched, setTouched] = useState(false);

  const emailInvalid = touched && !email.includes("@");
  const pwdInvalid = touched && password.length < 6;

  const handleSubmit = async (e) => {
    e.preventDefault();
    setTouched(true);
    if (emailInvalid || pwdInvalid) return;
    await onSubmit({ email, password });
  };

  return (
    <form className="max-w-md mx-auto mt-20 p-6 border rounded-lg bg-white shadow" onSubmit={handleSubmit}>
      <h2 className="mb-6 text-2xl font-bold text-center text-gray-800">{title}</h2>
      <div className="flex flex-col gap-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Email</label>
          <input
            type="email"
            value={email}
            onChange={e => setEmail(e.target.value)}
            placeholder="you@example.com"
            className={`w-full px-4 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 ${emailInvalid ? "border-red-500" : "border-gray-300"}`}
          />
          {emailInvalid && <div className="text-red-500 text-xs mt-1">Enter a valid email.</div>}
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Password</label>
          <input
            type="password"
            value={password}
            onChange={e => setPassword(e.target.value)}
            placeholder="Min 6 chars"
            className={`w-full px-4 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 ${pwdInvalid ? "border-red-500" : "border-gray-300"}`}
          />
          {pwdInvalid && <div className="text-red-500 text-xs mt-1">Password must be at least 6 characters.</div>}
        </div>
        <button
          type="submit"
          className={`w-full py-2 px-4 bg-teal-600 text-white rounded-lg font-semibold shadow hover:bg-teal-700 transition-all duration-150 ${isLoading ? "opacity-50 cursor-not-allowed" : ""}`}
          disabled={isLoading}
        >
          {isLoading ? "Processing..." : submitText}
        </button>
      </div>
    </form>
  );
}

