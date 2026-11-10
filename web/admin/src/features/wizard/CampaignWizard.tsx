
import { useRef, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { api } from '../../lib/api'
import { Button, Card } from '../../components/ui'
import {
  emptyDraft,
  toRequest,
  validateStep,
  type CampaignDraft,
  type DraftErrors,
} from './campaignDraft'
import { StepDetails } from './StepDetails'
import { StepSchedule } from './StepSchedule'
import { StepOutlets } from './StepOutlets'
import { StepReview } from './StepReview'

const STEPS = ['Details', 'Schedule & budget', 'Outlets', 'Review']

export function CampaignWizard() {
  const navigate = useNavigate()
  const [step, setStep] = useState(0)
  const [draft, setDraft] = useState<CampaignDraft>(emptyDraft)
  const [errors, setErrors] = useState<DraftErrors>({})
  const [submitError, setSubmitError] = useState<string | null>(null)
  const [submitting, setSubmitting] = useState(false)

  // Reuse this key if the request is retried after an uncertain response.
  const requestKey = useRef(crypto.randomUUID())

  function updateDraft(values: Partial<CampaignDraft>) {
    setDraft(current => ({ ...current, ...values }))
  }

  function next() {
    const stepErrors = validateStep(step, draft)
    setErrors(stepErrors)

    if (Object.keys(stepErrors).length > 0) return

    setStep(current => Math.min(current + 1, STEPS.length - 1))
  }

  function back() {
    setErrors({})
    setStep(current => Math.max(current - 1, 0))
  }

  async function submit() {
    if (submitting) return

    const allErrors = [0, 1, 2].reduce(
      (combined, currentStep) => ({
        ...combined,
        ...validateStep(currentStep, draft),
      }),
      {} as DraftErrors,
    )

    if (Object.keys(allErrors).length > 0) {
      setErrors(allErrors)
      const firstInvalidStep = [0, 1, 2].find(
        currentStep => Object.keys(validateStep(currentStep, draft)).length > 0,
      )
      if (firstInvalidStep !== undefined) setStep(firstInvalidStep)
      return
    }

    setSubmitting(true)
    setSubmitError(null)

    try {
      const created = await api<{ id: number }>('/api/campaigns', {
        method: 'POST',
        headers: { 'Idempotency-Key': requestKey.current },
        body: JSON.stringify(toRequest(draft)),
      })

      navigate(`/campaigns/${created.id}`)
    } catch {
      setSubmitError('Campaign could not be created. Please try again.')
      setSubmitting(false)
    }
  }

  return (
    <div className="space-y-4">
      <ol className="grid grid-cols-4 gap-2 text-xs sm:text-sm">
        {STEPS.map((label, index) => (
          <li
            key={label}
            className={`rounded-md px-2 py-2 text-center ${
              index === step
                ? 'bg-brand-50 font-semibold text-brand-700'
                : 'bg-slate-50 text-slate-500'
            }`}
          >
            {index + 1}. {label}
          </li>
        ))}
      </ol>

      <Card className="p-4 sm:p-6">
        {step === 0 && (
          <StepDetails
            draft={draft}
            errors={errors}
            onChange={updateDraft}
            onNext={next}
          />
        )}

        {step === 1 && (
          <StepSchedule
            draft={draft}
            errors={errors}
            onChange={updateDraft}
            onNext={next}
            onBack={back}
          />
        )}

        {step === 2 && (
          <StepOutlets
            selected={draft.outletIds}
            error={errors.outletIds}
            onChange={outletIds => updateDraft({ outletIds })}
            onNext={next}
            onBack={back}
          />
        )}

        {step === 3 && (
          <>
            {submitError && (
              <p role="alert" className="mb-4 rounded-md bg-red-50 p-3 text-sm text-red-700">
                {submitError}
              </p>
            )}

            <StepReview draft={draft} />

            <div className="mt-6 flex justify-between gap-3">
              <Button variant="secondary" onClick={back} disabled={submitting}>
                Back
              </Button>
              <Button onClick={submit} disabled={submitting}>
                {submitting ? 'Creating...' : 'Create campaign'}
              </Button>
            </div>
          </>
        )}
      </Card>
    </div>
  )
}
