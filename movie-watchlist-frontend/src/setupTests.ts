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

// Polyfill for BroadcastChannel (required for MSW v2)
(global as any).BroadcastChannel = class BroadcastChannel {
  name: string;
  constructor(name: string) {
    this.name = name;
  }
  postMessage() {}
  close() {}
  addEventListener() {}
  removeEventListener() {}
  dispatchEvent() { return true; }
};

// Polyfill for TransformStream (required for MSW v2)
(global as any).TransformStream = class TransformStream {
  readable: any;
  writable: any;
  constructor() {
    this.readable = {};
    this.writable = {};
  }
};
