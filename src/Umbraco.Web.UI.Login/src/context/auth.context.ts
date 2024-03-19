import type {
  LoginRequestModel,
  LoginResponse,
  ResetPasswordResponse,
  ValidatePasswordResetCodeResponse,
  NewPasswordResponse,
  PasswordConfigurationModel, ValidateInviteCodeResponse
} from "../types.js";
import {UmbAuthRepository} from './auth.repository.js';

export class UmbAuthContext {
  readonly supportsPersistLogin = false;
  disableLocalLogin = false;
  twoFactorView = '';
  isMfaEnabled = false;
  mfaProviders: string[] = [];
  passwordConfiguration?: PasswordConfigurationModel;

  #authRepository = new UmbAuthRepository();

  #returnPath = '';

  set returnPath(value: string) {
    this.#returnPath = value;
  }

  /**
   * Gets the return path from the query string.
   *
   * It will first look for a `ReturnUrl` parameter, then a `returnPath` parameter, and finally the `returnPath` property.
   *
   * @returns The return path from the query string.
   */
  get returnPath(): string {
    const params = new URLSearchParams(window.location.search);
    let returnPath = params.get('ReturnUrl') ?? params.get('returnPath') ?? this.#returnPath;

    // If return path is empty, return an empty string.
    if (!returnPath) {
      return '';
    }

    // Safely check that the return path is valid and doesn't link to an external site.
    const url = new URL(returnPath, window.location.origin);

    if (url.origin !== window.location.origin) {
      return '';
    }

    return url.toString();
  }

  async login(data: LoginRequestModel): Promise<LoginResponse> {
    return this.#authRepository.login(data);
  }

  async resetPassword(username: string): Promise<ResetPasswordResponse> {
    return this.#authRepository.resetPassword(username);
  }

  async validatePasswordResetCode(userId: string, resetCode: string): Promise<ValidatePasswordResetCodeResponse> {
    return this.#authRepository.validatePasswordResetCode(userId, resetCode);
  }

  async newPassword(password: string, resetCode: string, userId: string): Promise<NewPasswordResponse> {
    return this.#authRepository.newPassword(password, resetCode, userId);
  }

  async newInvitedUserPassword(password: string, token: string, userId: string): Promise<NewPasswordResponse> {
    return this.#authRepository.newInvitedUserPassword(password, token, userId);
  }

  async validateInviteCode(token: string, userId: string): Promise<ValidateInviteCodeResponse> {
    return this.#authRepository.validateInviteCode(token, userId);
  }

  validateMfaCode(code: string, provider: string): Promise<LoginResponse> {
    return this.#authRepository.validateMfaCode(code, provider);
  }
}

export const umbAuthContext = new UmbAuthContext();
