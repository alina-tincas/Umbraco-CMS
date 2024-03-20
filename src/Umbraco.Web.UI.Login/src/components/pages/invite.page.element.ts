import type { UUIButtonState } from '@umbraco-cms/backoffice/external/uui';
import { html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

import { umbAuthContext } from '../../context/auth.context.js';

@customElement('umb-invite-page')
export default class UmbInvitePageElement extends UmbLitElement {
  #token = '';
  #userId = '';

  @state()
  state: UUIButtonState = undefined;

  @state()
  error = '';

  @state()
  loading = true;

  constructor() {
    super();
    this.#init();
  }

  async #init() {
    const urlParams = new URLSearchParams(window.location.search);
    const token = urlParams.get('inviteCode');
    const userId = urlParams.get('userId');

    if (!token || !userId) {
      this.error = 'The invite has expired or is invalid';
      this.loading = false;
      return;
    }

    this.#token = token;
    this.#userId = userId;

    const response = await umbAuthContext.validateInviteCode(this.#token, this.#userId);

    if (response.error) {
      this.error = response.error;
      this.loading = false;
      return;
    }

    umbAuthContext.passwordConfiguration = response.data?.passwordConfiguration;
    this.loading = false;
  }

  async #onSubmit(event: CustomEvent) {
    event.preventDefault();
    const password = event.detail.password;

    if (!password) return;

    this.state = 'waiting';
    const response = await umbAuthContext.newInvitedUserPassword(password, this.#token, this.#userId);

    if (response.error) {
      this.error = response.error;
      this.state = 'failed';
      return;
    }

    this.state = 'success';
    window.location.href = umbAuthContext.returnPath;
  }

  render() {
    return this.loading ? html`<uui-loader-bar></uui-loader-bar>` : (
      this.error
        ? html`
          <umb-error-layout
            header=${this.localize.term('general_error')}
            message=${this.error ?? this.localize.term('errors_defaultError')}>
          </umb-error-layout>`
      : html`
        <umb-new-password-layout
          @submit=${this.#onSubmit}
          is-invite
          .userId=${this.#userId}
          .state=${this.state}
          .error=${this.error}></umb-new-password-layout>`
    );
  }
}

declare global {
  interface HTMLElementTagNameMap {
    'umb-invite-page': UmbInvitePageElement;
  }
}
