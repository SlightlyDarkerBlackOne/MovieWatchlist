// jest-dom adds custom jest matchers for asserting on DOM nodes.
// allows you to do things like:
// expect(element).toHaveTextContent(/react/i)
// learn more: https://github.com/testing-library/jest-dom
import '@testing-library/jest-dom';

// Polyfill for TextEncoder/TextDecoder (required for react-router v7+)
import { TextEncoder, TextDecoder } from 'util';

// Type assertion to bypass type incompatibility
(global as any).TextEncoder = TextEncoder;
(global as any).TextDecoder = TextDecoder;
