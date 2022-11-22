export default interface Options {
  secure?: boolean;
  port?: number;
  keyfile?: string;
  certfile?: string;
  http?: boolean;
  mode?: string;
  logging?: string;
}